using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private int _waveEnemyCount;

    public bool isWaitingToEnd;

    public CameraMovement cameraMovement;

    // Start is called before the first frame update
    private void Awake()
    {
        if (Manager.instance == null)
        {
            StartCoroutine(StartHere());
        }
        else
        {
            Manager.SetWaveManager(this);
        }
    }

    private IEnumerator StartHere()
    {
        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        foreach (Entity e in FindObjectsOfType<Entity>())
        {
            if (e is Enemy en)
            {
                en.gameObject.SetActive(false);
            }
        }
        Manager.SetWaveManager(this);
    }

    public void AddEnemyToPool(Entity e)
    {
        _waveEnemyCount++;
        cameraMovement.SlowDown();
        e.OnDestroyEvent.AddListener((et) =>
        {
            _waveEnemyCount--;
            if (_waveEnemyCount == 0)
            {
                cameraMovement.SpeedUp();
            }
        });
    }

    // Update is called once per frame
    private void Update()
    {
        if (isWaitingToEnd && _waveEnemyCount == 0)
        {
            Manager.instance.NextWave();
            isWaitingToEnd = false;
            Destroy(this);
        }
    }
}