using System;
using System.Collections.Generic;
using Game.Scripts.Characters;
using Game.Scripts.Characters.Bullets;
using Game.Scripts.Levels;
using UnityEngine;

namespace Game.Scripts.Upgrades
{
    public enum CampaignUpgradeKind
    {
        ArrowDamage, Health, ArrowCount, FireDamage, FireLifetime, Reload,
        PoisonImpact, PoisonDamage, Pierce, PoisonDuration, Critical, Shield, Heal, Coins
    }

    [Serializable]
    public class UpgradeRank
    {
        public CampaignUpgradeKind Kind;
        public int Rank;
    }

    [RequireComponent(typeof(PlayerBulletUpgradeCollection), typeof(Health))]
    public class CampaignUpgrades : MonoBehaviour
    {
        private List<UpgradeRank> _ranks = new();
        private PlayerBulletUpgradeCollection _bullets;
        private Health _health;
        public int UnlockedStage { get; private set; } = 1;
        public int ShieldCharges { get; private set; }
        public int Rerolls { get; private set; }
        public event Action Changed;
        public float ReloadMultiplier => 1f - .05f * Rank(CampaignUpgradeKind.Reload);
        public float CriticalChance => .05f * Rank(CampaignUpgradeKind.Critical);
        public int Pierce => Rank(CampaignUpgradeKind.Pierce);

        private void Awake()
        {
            _bullets = GetComponent<PlayerBulletUpgradeCollection>();
            _health = GetComponent<Health>();
            _health.TryBlockDamage = TryBlock;
        }

        public int Rank(CampaignUpgradeKind kind) => _ranks.Find(x => x.Kind == kind)?.Rank ?? 0;
        public List<UpgradeRank> CaptureRanks() => _ranks.ConvertAll(x => new UpgradeRank { Kind = x.Kind, Rank = x.Rank });
        public void Restore(List<UpgradeRank> ranks, int shields, int rerolls, int unlocked)
        {
            _ranks = ranks?.ConvertAll(x => new UpgradeRank { Kind = x.Kind, Rank = x.Rank }) ?? new();
            ShieldCharges = Mathf.Clamp(shields, 0, Rank(CampaignUpgradeKind.Shield));
            Rerolls = Mathf.Max(0, rerolls);
            UnlockedStage = Mathf.Clamp(unlocked, 1, 20);
            Changed?.Invoke();
        }

        public void BeginStage(int unlockedStage, bool refill)
        {
            UnlockedStage = Mathf.Max(UnlockedStage, unlockedStage);
            if (!_bullets.IsUnlocked(BulletType.Arrow)) _bullets.Unlock(BulletType.Arrow);
            if (UnlockedStage >= 4 && !_bullets.IsUnlocked(BulletType.FireArrow)) _bullets.Unlock(BulletType.FireArrow);
            if (UnlockedStage >= 7 && !_bullets.IsUnlocked(BulletType.PoisonArrow)) _bullets.Unlock(BulletType.PoisonArrow);
            if (refill)
            {
                _health.ResetHealth();
                ShieldCharges = Rank(CampaignUpgradeKind.Shield);
                Rerolls = CampaignRules.Rerolls(UnlockedStage);
            }
            Changed?.Invoke();
        }

        public static float ArrowMultiplier(int count) => count <= 1 ? 1f : count == 2 ? .7f : .55f;
        public static float PierceMultiplier(int targetIndex) => Mathf.Pow(.6f, targetIndex);
        public static int UnlockStage(CampaignUpgradeKind kind) => kind switch
        {
            CampaignUpgradeKind.ArrowCount => 3, CampaignUpgradeKind.FireDamage => 4,
            CampaignUpgradeKind.FireLifetime => 5, CampaignUpgradeKind.Reload => 6,
            CampaignUpgradeKind.PoisonImpact => 7, CampaignUpgradeKind.PoisonDamage => 8,
            CampaignUpgradeKind.Pierce => 9, CampaignUpgradeKind.PoisonDuration => 11,
            CampaignUpgradeKind.Critical => 12, CampaignUpgradeKind.Shield => 13, _ => 1
        };

        public static int MaxRank(CampaignUpgradeKind kind, int stage) => kind switch
        {
            CampaignUpgradeKind.ArrowCount => stage >= 16 ? 2 : 1,
            CampaignUpgradeKind.Pierce => stage >= 18 ? 2 : 1,
            CampaignUpgradeKind.Shield => stage >= 17 ? 2 : 1,
            CampaignUpgradeKind.FireLifetime or CampaignUpgradeKind.PoisonDuration => 3,
            CampaignUpgradeKind.Reload or CampaignUpgradeKind.Critical => 4,
            _ => int.MaxValue
        };

        // Existing save bonuses count towards capped upgrades without reducing their value.
        public int EffectiveRank(CampaignUpgradeKind kind)
        {
            if (kind == CampaignUpgradeKind.ArrowCount) return _bullets.Get(BulletType.Arrow).CountBonus;
            if (kind == CampaignUpgradeKind.FireLifetime)
            {
                var entry = _bullets.Get(BulletType.FireArrow);
                return Mathf.CeilToInt(entry.LifeTimeBonus / (entry.BaseLifeTime * .15f) - .0001f);
            }
            if (kind == CampaignUpgradeKind.PoisonDuration)
            {
                var entry = _bullets.Get(BulletType.PoisonArrow);
                return Mathf.CeilToInt(entry.LifeTimeBonus / (entry.BaseLifeTime * .2f) - .0001f);
            }
            return Rank(kind);
        }

        public bool CanApply(CampaignUpgradeKind kind)
        {
            if (UnlockedStage < UnlockStage(kind)) return false;
            if (kind == CampaignUpgradeKind.Heal) return _health.CurrentCount < _health.MaxCount;
            return EffectiveRank(kind) < MaxRank(kind, UnlockedStage);
        }

        public bool Apply(CampaignUpgradeKind kind)
        {
            if (!CanApply(kind)) return false;
            var arrow = _bullets.Get(BulletType.Arrow);
            var fire = _bullets.Get(BulletType.FireArrow);
            var poison = _bullets.Get(BulletType.PoisonArrow);
            switch (kind)
            {
                case CampaignUpgradeKind.ArrowDamage: arrow.AddDamage(arrow.BaseDamage * .1f); break;
                case CampaignUpgradeKind.Health: _health.IncreaseMaxHealth(_health.BaseMax * .1f); break;
                case CampaignUpgradeKind.ArrowCount: arrow.AddCount(1); break;
                case CampaignUpgradeKind.FireDamage: fire.AddDamage(fire.BaseDamage * .1f); break;
                case CampaignUpgradeKind.FireLifetime: fire.AddLifeTime(fire.BaseLifeTime * .15f); break;
                case CampaignUpgradeKind.PoisonImpact: poison.AddDamage(poison.BaseDamage * .1f); break;
                case CampaignUpgradeKind.PoisonDamage: poison.AddPoisonDamage(poison.BasePoisonDamage * .2f); break;
                case CampaignUpgradeKind.PoisonDuration: poison.AddLifeTime(poison.BaseLifeTime * .2f); break;
                case CampaignUpgradeKind.Shield: ShieldCharges++; break;
                case CampaignUpgradeKind.Heal: _health.Heal(Mathf.CeilToInt(_health.MaxCount * .25f)); return true;
                case CampaignUpgradeKind.Coins: return true;
            }
            var rank = _ranks.Find(x => x.Kind == kind);
            if (rank == null) _ranks.Add(new UpgradeRank { Kind = kind, Rank = 1 });
            else rank.Rank++;
            _bullets.NotifyLoadedFromSave();
            Changed?.Invoke();
            return true;
        }

        public bool SpendReroll()
        {
            if (Rerolls <= 0) return false;
            Rerolls--;
            Changed?.Invoke();
            return true;
        }

        private bool TryBlock()
        {
            if (ShieldCharges <= 0) return false;
            ShieldCharges--;
            Changed?.Invoke();
            return true;
        }

        public string Preview(CampaignUpgradeKind kind)
        {
            var a = _bullets.Get(BulletType.Arrow);
            var f = _bullets.Get(BulletType.FireArrow);
            var p = _bullets.Get(BulletType.PoisonArrow);
            string Change(float current, float delta) => $"{current:0.##} → {current + delta:0.##}";
            return kind switch
            {
                CampaignUpgradeKind.ArrowDamage => Change(a.Damage, a.BaseDamage * .1f),
                CampaignUpgradeKind.Health => Change(_health.MaxCount, _health.BaseMax * .1f),
                CampaignUpgradeKind.ArrowCount => Change(a.Count, 1),
                CampaignUpgradeKind.FireDamage => Change(f.Damage, f.BaseDamage * .1f),
                CampaignUpgradeKind.FireLifetime => Change(f.LifeTime, f.BaseLifeTime * .15f),
                CampaignUpgradeKind.PoisonImpact => Change(p.Damage, p.BaseDamage * .1f),
                CampaignUpgradeKind.PoisonDamage => Change(p.PoisonDamage, p.BasePoisonDamage * .2f),
                CampaignUpgradeKind.PoisonDuration => Change(p.LifeTime, p.BaseLifeTime * .2f),
                CampaignUpgradeKind.Reload => Change((1 - ReloadMultiplier) * 100, 5) + "%",
                CampaignUpgradeKind.Critical => Change(CriticalChance * 100, 5) + "%",
                CampaignUpgradeKind.Pierce or CampaignUpgradeKind.Shield => Change(Rank(kind), 1),
                _ => ""
            };
        }
    }
}
