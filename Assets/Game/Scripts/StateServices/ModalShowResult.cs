namespace Game.Scripts.StateServices
{
    public enum ModalShowResult
    {
        /// <summary>Модалка показана сразу</summary>
        Shown,
        /// <summary>Другая модалка открыта — запрос добавлен в очередь</summary>
        Queued,
        /// <summary>Такая же модалка уже открыта или уже в очереди</summary>
        AlreadyPending,
    }
}