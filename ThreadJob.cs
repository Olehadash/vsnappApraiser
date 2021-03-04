using System.Collections;
using System.Collections.Generic;
using UnityGoogleDrive;
using UnityGoogleDrive.Data;

public delegate void ThreadMethod(List<byte[]> images);
public delegate void ThreadMethodWithString(string line);

public class ThreadedJob
{
    private bool m_IsDone = false;
    private object m_Handle = new object();
    private System.Threading.Thread m_Thread = null;
    public ThreadMethod method;
    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }
    List<byte[]> images;
    public virtual void Start(List<byte[]> images)
    {
        this.images = images;
        m_Thread = new System.Threading.Thread(Run);
        m_Thread.Start();
    }
    public virtual void Abort()
    {
        m_Thread.Abort();
    }

    protected virtual void ThreadFunction(List<byte[]> images) {
        if(method!=null)
            method(images);
    }

    protected virtual void OnFinished() { }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }
    public IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return null;
        }
    }
    private void Run()
    {
        ThreadFunction(images);
        IsDone = true;
    }
}