using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WaveUIManager : MonoBehaviour
{
    public GameObject TitleAndDescriptionContainer;
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text descriptionText;

    public GameObject scoreTextContainer;
    public GameObject hitsTakenTextContainer;
    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text hitsTakenText;

    public IEnumerator ShowWaveClear(int waveScore, int hitsTaken)
    {
        TitleAndDescriptionContainer.gameObject.SetActive(true);
        titleText.SetText("WAVE CLEAR");

        AudioManager.PlaySFX(SFX.LevelWin);

        yield return new WaitForSeconds(0.5f);

        scoreText.SetText(waveScore.ToString());
        hitsTakenText.SetText(hitsTaken.ToString());

        scoreTextContainer.SetActive(true);
        scoreTextContainer.transform.localScale = new Vector3(0, 1, 1);
        scoreTextContainer.transform.DOScaleX(1, 0.25f);

        hitsTakenTextContainer.SetActive(true);
        hitsTakenTextContainer.transform.localScale = new Vector3(0, 1, 1);
        hitsTakenTextContainer.transform.DOScaleX(1, 0.25f);

        yield return new WaitForSeconds(2.5f);

        scoreTextContainer.transform.DOScaleX(0, 0.25f);
        hitsTakenTextContainer.transform.DOScaleX(0, 0.25f);

        yield return new WaitForSeconds(0.25f);

        scoreTextContainer.SetActive(false);
        hitsTakenTextContainer.SetActive(false);

        TitleAndDescriptionContainer.gameObject.SetActive(false);
    }

    public IEnumerator ShowWaveStart(string waveName, string waveFlavourName)
    {
        titleText.SetText(waveName);
        descriptionText.SetText(waveFlavourName);

        TitleAndDescriptionContainer.gameObject.SetActive(true);
        TitleAndDescriptionContainer.transform.localScale = Vector3.zero;
        TitleAndDescriptionContainer.transform.DOScale(1, 0.25f);

        AudioManager.PlaySFX(SFX.LevelStart);
        yield return new WaitForSeconds(2.25f);
        TitleAndDescriptionContainer.transform.DOScale(0, 0.25f);
        yield return new WaitForSeconds(0.25f);

        TitleAndDescriptionContainer.transform.localScale = Vector3.one;
        TitleAndDescriptionContainer.SetActive(false);
    }
}