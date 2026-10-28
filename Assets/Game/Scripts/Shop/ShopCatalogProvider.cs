namespace Game.Scripts.Shop
{
    public sealed class ShopCatalogProvider
    {
        public ShopCatalog Catalog { get; private set; }

        public bool HasCatalog => Catalog != null;

        public void SetCatalog(ShopCatalog catalog)
        {
            Catalog = catalog;
        }
    }
}
