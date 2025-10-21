using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }
    public Bullet bulletPrefab;
    public int initialSize = 20;

    Queue<Bullet> pool = new Queue<Bullet>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < initialSize; i++)
        {
            CreateBullet();
        }
    }

    Bullet CreateBullet()
    {
        var b = Instantiate(bulletPrefab, transform);
        b.gameObject.SetActive(false);
        pool.Enqueue(b);
        return b;
    }

    public Bullet GetBullet()
    {
        if (pool.Count == 0) CreateBullet();
        var b = pool.Dequeue();
        b.gameObject.SetActive(true);
        return b;
    }

    public void ReturnBullet(Bullet b)
    {
        b.gameObject.SetActive(false);
        pool.Enqueue(b);
    }
}
