using UnityEngine;

public class PKMNParticleManager : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _emotionParticles;
    [SerializeField]
    private ParticleSystem _shinyParticles;

    public void SetShiny(bool isShiny)
    {
       if (isShiny)
        {
            _shinyParticles.Play();
        }
        else
        {
            Destroy(_shinyParticles.gameObject);
            _shinyParticles = null;
        }
    }

    public void PlayParticle(ParticleType type)
    {
        switch(type)
        {
            case ParticleType.Happy:
                _emotionParticles.Stop();
                _emotionParticles.Play();
                break;
        }
    }

}
public enum ParticleType
{
    Happy,
    Sad,
    Angry
}
