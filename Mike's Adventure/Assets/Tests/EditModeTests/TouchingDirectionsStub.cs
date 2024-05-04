namespace Assets.Tests
{
    internal class TouchingDirectionsStub : ITouchingDirections
    {
        public bool IsGrounded { get;set; } = true;

        public bool IsOnWall => WallDirection != WallDirection.None;
        public WallDirection WallDirection { get; set; } = WallDirection.None;

        public bool IsOnCeiling { get; set; } = false;
    }
}
