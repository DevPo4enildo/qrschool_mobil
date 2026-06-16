namespace qrschool_mobil
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ScanPage), typeof(ScanPage));
            Routing.RegisterRoute(nameof(AddEquipmentPage), typeof(AddEquipmentPage));
            Routing.RegisterRoute(nameof(EquipmentListPage), typeof(EquipmentListPage));
        }
    }
}
