using UnityEngine;

namespace NonsensicalKit.UGUI.Media.Samples
{
    public class AudioManagerDemo : MonoBehaviour
    {
        [SerializeField] private AudioManager m_audioManager;
        [SerializeField] private string m_audioUrl = "https://m704.music.126.net/20241010114726/a624e2ba032640635ed478843218d695/jdyyaac/obj/w5rDlsOJwrLDjj7CmsOj/17553712823/ca82/a6e2/d901/90edf9f1d5bba464e5b8d145cdbd1526.m4a?authSecret=000001927472fffd18cd0a8aadb70006";

        public void PlayTest()
        {
            m_audioManager.PlayAudio(m_audioUrl);
        }
    }
}
