using UnityEngine;

namespace NonsensicalKit.UGUI.Media.Samples
{
    public class AudioManagerDemo : MonoBehaviour
    {
        [SerializeField] private AudioManager m_audioManager;
        [SerializeField] private string m_audioUrl = "http://music.163.com/song/media/outer/url?id=1270558.mp3";

        public void PlayTest()
        {
            m_audioManager.PlayAudio(m_audioUrl);
        }
    }
}
