using UnityEngine;

namespace Assets.GeneralScripts
{
    public interface ITime
    {
        /// <summary>
        /// The interval, in seconds, from the last frame to the current one.
        /// </summary>
        float DeltaTime { get; }
    }

    public class UnityTime : ITime
    {
        /// <inheritdoc />
        public float DeltaTime => Time.deltaTime;
    }
}
