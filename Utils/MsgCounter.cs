using System;
using System.Collections.Generic;
using System.Threading;

public class MsgCounter
{
    private long totalCount;
    private long messagesPerSecond;
    private Queue<DateTime> messageQueue;
    private Timer timer;

    public MsgCounter() {
        totalCount = 0;
        messagesPerSecond = 0;
        messageQueue = new Queue<DateTime>();

        timer = new Timer(UpdateMessagesPerSecond, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public void Increment() {
        Interlocked.Increment(ref totalCount);
        lock (messageQueue) {
            messageQueue.Enqueue(DateTime.Now);
        }
    }

    private void UpdateMessagesPerSecond(object state) {
        DateTime threshold = DateTime.Now.AddSeconds(-1);
        lock (messageQueue) {
            while (messageQueue.Count > 0 && messageQueue.Peek() < threshold) {
                messageQueue.Dequeue();
            }

            messagesPerSecond = messageQueue.Count;
        }
    }

    public long GetTotalCount() {
        return Interlocked.Read(ref totalCount);
    }

    public long GetMessagesPerSecond() {
        return Interlocked.Read(ref messagesPerSecond);
    }

}
