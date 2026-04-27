using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Collectable Parameters")]
    [SerializeField] private int Points;
    [SerializeField] private int Rotate = 1;
    private ScoreManager scoreManager;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Rotate, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            scoreManager = collision.gameObject.GetComponentInChildren<ScoreManager>();

            scoreManager.CollectablePoints += Points;
            scoreManager.CollecablesCollected++;
            Debug.Log(Points + "+");

            Destroy(this.gameObject);
        }

    }

}
