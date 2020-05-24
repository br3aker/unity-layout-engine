namespace SoftKata.Editor {
    public interface IRepaintable {
        void Repaint();

        void RegisterRepaintRequest();
        void UnregisterRepaintRequest();
    }
}
