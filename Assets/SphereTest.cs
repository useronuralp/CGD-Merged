using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
namespace Game
{
    public class SphereTest : MonoBehaviourPunCallbacks, IPunObservable
    {
        private Rigidbody _rigidbody;
        private Vector3 _networkPosition;
        private Quaternion _networkRotation;
        [SerializeField]
        private float _LerpSmoothness = 0.0f; //TODO: Figure out why this value causes jittering on the sphere.
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                _rigidbody.position = Vector3.MoveTowards(_rigidbody.position, _networkPosition, Time.fixedDeltaTime * _LerpSmoothness);
                _rigidbody.rotation = Quaternion.RotateTowards(_rigidbody.rotation, _networkRotation, Time.fixedDeltaTime * 100.0f);
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            //if(!collision.transform.CompareTag("Player"))
            //{
            //    return;
            //}
            //
            //RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            //{
            //    Receivers = ReceiverGroup.All,
            //    CachingOption = EventCaching.DoNotCache
            //};
            //
            //ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions
            //{
            //    Reliability = true
            //};
            //
            //object[] customData = new object[1];
            //Vector3 direction = collision.rigidbody.transform.position - transform.position;
            //customData[0] = direction;
            //PhotonNetwork.RaiseEvent(1, customData, raiseEventOptions, sendOptions);
            //Debug.Log("Spehere: Sent event");
        }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //Sending and recieving rigidbody data like this to compensate for lag!
        {
            //IMPORTANT: Only the owener of the object writes to the stream. If the object is a Room Object, like this Sphere, only the MasterClient will be able to write to the stream.
            //IMPORTANT: You can use InstantiteRoomObejct() to instantiate Room objects that have no creators.
            if (stream.IsWriting)
            {
                //IMPORTANT: The order you are sending the data MUST be the same in the recieving part.
                Debug.Log("Serialized data");
                stream.SendNext(_rigidbody.position);
                stream.SendNext(_rigidbody.rotation);
                stream.SendNext(_rigidbody.velocity);
            }
            else
            {
                //The recieving order MUST be the same as the sending order.
                Debug.Log("Read data and applied.");
                _networkPosition = (Vector3)stream.ReceiveNext();
                _networkRotation = (Quaternion)stream.ReceiveNext();
                _rigidbody.velocity = (Vector3)stream.ReceiveNext();

                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                _networkPosition += (_rigidbody.velocity * lag);
            }
        }
    }
}
