/* Radical Forge Copyright (c) 2017 All Rights Reserved
   </copyright>
   <author>Frederic Babord</author>
   <date>01st September 2017</date>
   <summaryA collectable object triggereed either by collision of trigger.
   REQUIRES: Collection Trigger in scene</summary>*/

using UnityEngine;

namespace RadicalForge.Gameplay
{
    
    public class Collectable : MonoBehaviour
    {
        public static int m_pickupsCollected;
		public ParticleSystem particleToStop;
		public ParticleSystem particleToSpawn;
        
        // Update is called once per frame
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var ct = FindAnyObjectByType<CollectionTrigger>();
				if (ct) {
                    //Debug.Log("collecting");
					particleToStop.Stop ();
					particleToSpawn.Play ();
                    m_pickupsCollected += 1;
                    //Debug.Log(m_pickupsCollected);
                    ct.Collect (gameObject);
					GetComponent<Collider> ().enabled = false;
				}
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.CompareTag("Player"))
            {
                var ct = FindObjectOfType<CollectionTrigger>();
				if (ct) {
					particleToStop.Stop ();
					particleToSpawn.Play ();
                    m_pickupsCollected += 1;
                    ct.Collect (gameObject);
					GetComponent<Collider> ().enabled = false;
				}
            }
        }
    }

}