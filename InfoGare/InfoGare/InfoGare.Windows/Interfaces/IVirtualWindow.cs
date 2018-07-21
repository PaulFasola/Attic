namespace Fasolib.Interfaces
{
    /// <summary>
    ///  Interface for the Virtual Window's module
    /// </summary>
    public interface IVirtualWindow 
    {
        bool IsVisible { get; set; }
        string Id { get; set; }
    }
}
