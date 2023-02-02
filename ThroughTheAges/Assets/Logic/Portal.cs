using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public static System.Action<Player.PlayerState> enteredPortal;
    
    public Player.PlayerState state;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            enteredPortal?.Invoke(state);
            player.state = state;
            GetComponent<Collider>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(true);
            StartCoroutine(ScaleDown(transform.GetChild(1)));
        }
    }

    IEnumerator ScaleDown(Transform transform)
    {
        float t = 0f;
        Vector3 startScale = transform.localScale;
        while (t < 0.5f)
        {
            transform.localScale = startScale * (1f - t / 0.5f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
