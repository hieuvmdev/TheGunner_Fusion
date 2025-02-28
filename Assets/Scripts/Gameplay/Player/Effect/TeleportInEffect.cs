using System.Collections;
using UnityEngine;
public class TeleportInEffect : MonoBehaviour
{

	[Header("Time Settings")] 
	[SerializeField] private float timeBeforeParticles = 0.1f;

	[SerializeField] private float timeDelayImpactParticles = 0.2f;

	[SerializeField] private ParticleSystem teleportBeamParticle;
	[SerializeField] private ParticleSystem teleportImpactParticle;

    private Player _player;
	private bool _endTeleportation;

	public void Initialize(Player player)
	{
		_player = player;
		ColorChanger.ChangeColor(transform, player.PlayerColor);
		ResetTeleporter();
	}

	public void EndTeleport()
	{
		_endTeleportation = true;
	}

	private void ResetTeleporter()
	{
		_endTeleportation = false;
		teleportBeamParticle.Stop();
		teleportImpactParticle.Stop();
	}

	public void StartTeleport()
	{
		ResetTeleporter();
		StartCoroutine(TeleportIn());
	}

	private IEnumerator TeleportIn()
	{
		yield return new WaitForSeconds(timeBeforeParticles);

		// Play the downwards beam
		teleportBeamParticle.Play();
		SoundManager.Instance.Play3DSFX(AudioEnum.TeleportIn, transform.position);
		yield return new WaitForSeconds(timeDelayImpactParticles);

		// Play impact particle
		teleportBeamParticle.Stop();
		teleportImpactParticle.Play();

	}
}
