namespace TableConverter.Interfaces.OverlayService
{
    public interface IOverlayService
    {
        public IDialogBuilder CreateDialog();

        public ICustomDialogBuilder CreateCustomDialog();

        public IMessageBoxBuilder CreateMessageBox();

        public IDrawerBuilder CreateDrawer();

        public ICustomDrawerBuilder CreateCustomDrawer();
    }
}
