namespace Assets.Tests
{
    internal class TouchingDirectionsStub : ITouchingDirections
    {
        public bool IsGrounded { get;set; } = true;

        public bool IsOnWall { get; set; } = false;

        public bool IsOnCeiling { get; set; } = false;
    }
}
