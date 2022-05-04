using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class Stamina : MonoBehaviourPunCallbacks
{
    private float m_MaxStamina = 100;
    public float m_CurrentStamina { get; set; } = 100;
    private float m_UpdateSpeedSeconds = 0.2f; //Used in coroutine to slowly animate health drops.
    private float m_StaminaRechargeCooldown = 2.0f;
    private float m_StaminaBarDisappearTimer = 2.0f;
    private float m_StaminaBarDisappearCooldown = 2.0f;
    private float m_StaminaRechargeTimer;

    private Game.PlayerManager m_PlayerManageScript;

    private  Image m_StaminaImage;
    private Image m_StaminaBackground;

    private void Awake()
    {
        m_StaminaImage = transform.Find("StaminaCanvas").Find("StaminaBackgroundImage").Find("StaminaImage").GetComponent<Image>();
        m_StaminaBackground = transform.Find("StaminaCanvas").Find("StaminaBackgroundImage").GetComponent<Image>();
        m_StaminaBarDisappearTimer = m_StaminaBarDisappearCooldown;
        m_StaminaRechargeTimer = m_StaminaRechargeCooldown;
    }
    private void Start()
    {
        m_PlayerManageScript = GetComponent<Game.PlayerManager>();
        m_StaminaBackground.enabled = false;
        m_StaminaImage.enabled = false;
    }
    private void Update()
    {
        m_CurrentStamina = Mathf.Clamp(m_CurrentStamina, 0, 100);
        if (m_CurrentStamina < 100) //Start displaying the stamina bar if it is being used. 
        {
            m_StaminaBarDisappearTimer = m_StaminaBarDisappearCooldown; //Fix the disappear timer.
            m_StaminaImage.enabled = true;
            m_StaminaBackground.enabled = true;
            m_StaminaRechargeTimer -= Time.deltaTime; //Also start this timer.
        }
        else if (m_CurrentStamina == 100) //If the stamina bar is at full capacity and has not been used for a certain duration, make it disappear.
        {
            m_StaminaRechargeTimer = m_StaminaRechargeCooldown; //Fix the recharge timer.
            m_StaminaBarDisappearTimer -= Time.deltaTime;
            if (m_StaminaBarDisappearTimer <= 0.0f)
            {
                m_StaminaBackground.enabled = false;
                m_StaminaImage.enabled = false;
            }
        }

        if (m_StaminaRechargeTimer <= 0)
        {
            if(m_PlayerManageScript.m_IsBulldog)
            {
                RechargeStamina(30.0f);
            }
            else
            {
                RechargeStamina(20.0f);
            }
        }
    }
    void RechargeStamina(float chargeRate)
    {
        m_CurrentStamina += chargeRate * Time.deltaTime;
        m_StaminaImage.fillAmount = m_CurrentStamina / m_MaxStamina; //This fillAmount field is clamped between [0, 1]. Therefore, I am passing the current percentage of the stamina to it.
    }
    public void ReduceStamina(float amount)                          //Change the current stamina first, then pass the percentage to the coroutine.
    {
        m_StaminaRechargeTimer = m_StaminaRechargeCooldown;         //Reset the recharge timer every time stamina is reduced, meaning whenever the character throws an attack.
        m_CurrentStamina -= amount;
        StartCoroutine(ChangeStaminaTo(m_CurrentStamina / m_MaxStamina));
    }
    private IEnumerator ChangeStaminaTo(float percentage)              //Only reason this is a Coroutine is because changing the stamina amount looks smooth this way, rather than sharp and instantenous decrease / increase in visuals.
    {
        float preChangePercentage = m_StaminaImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < m_UpdateSpeedSeconds)
        {
            elapsed += Time.deltaTime;
            m_StaminaImage.fillAmount = Mathf.Lerp(preChangePercentage, percentage, elapsed / m_UpdateSpeedSeconds);
            yield return null;
        }
        m_StaminaImage.fillAmount = percentage;
    }
}