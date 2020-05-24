namespace SoftKata.UnityEditor {
    public interface IRepaintable {
        void Repaint();

        void RegisterRepaintRequest();
        void UnregisterRepaintRequest();
    }
}
