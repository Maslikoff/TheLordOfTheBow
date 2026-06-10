namespace Game.Scripts.Environment.Effect
{
    public class CameraVignetteService : ICameraVignetteService
    {
        private readonly CameraVignetteEffect _vignetteEffect;
        
        public CameraVignetteService(CameraVignetteEffect vignetteEffect)
        {
            _vignetteEffect = vignetteEffect;
        }
        
        public void PlayPlayerHitVignette()
        {
            _vignetteEffect?.PlayPlayerHitVignette();
        }
        
        public void PlayEnemyHitVignette()
        {
            _vignetteEffect?.PlayEnemyHitVignette();
        }
    }
}