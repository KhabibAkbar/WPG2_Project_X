using UnityEngine;

public class CollectItem : MonoBehaviour
{
    public int itemID;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            ItemUIManager.instance.Collect(itemID);
            Destroy(gameObject);
        }
    }
}