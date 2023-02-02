using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    GameObject pieceParent;
    Rigidbody[] rbs;


    private void Start() {
        pieceParent = transform.GetChild(0).gameObject;
        pieceParent.SetActive(false);

        rbs = pieceParent.GetComponentsInChildren<Rigidbody>();
        foreach(var rb in rbs)
        {
            rb.isKinematic = true;
        }
    }

    public void Break(Vector3 position, float force)
    {
        pieceParent.SetActive(true);
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.AddForce((rb.position - position).normalized * force * (1f / (rb.position - position).magnitude), ForceMode.Impulse);
            // rb.detectCollisions = false;
            float t = 0f;
            Vector3 startScale = rb.transform.localScale;
            IEnumerator WaitForDestroy() {
                yield return new WaitForSeconds(5f);
                t += Time.deltaTime;
                while (t < 0.5f)
                {
                    if(rb != null)
                    rb.transform.localScale = startScale * (1f - t / 0.5f);
                    t += Time.deltaTime;
                    yield return null;
                }
                Destroy(rb.gameObject);
            }
            StartCoroutine(WaitForDestroy());
        }
    }
}
