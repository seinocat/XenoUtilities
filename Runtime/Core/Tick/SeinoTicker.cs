﻿using System;
using System.Collections.Generic;
using Seino.Utils.Singleton;
using UnityEngine;

namespace Seino.Utils.Tick
{
    /// <summary>
    /// 用于update执行逻辑
    /// </summary>
    public class SeinoTicker : MonoSingleton<SeinoTicker>
    {
        private Dictionary<long, Ticker> m_tickers = new();
        private Queue<long> m_updates = new();

        private void Update()
        {
            int count = m_updates.Count;
            while (count-- > 0)
            {
                long id = m_updates.Dequeue();
                if (!m_tickers.TryGetValue(id, out Ticker ticker)) continue;
                if (ticker.IsPause) continue;
                
                ticker.Update(Time.deltaTime);
                m_updates.Enqueue(id);
            }
        }
        
        /// <summary>
        /// 创建执行
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(Action<float> executor, float time = -1f, int framerate = 30)
        {
            long id = Guid.NewGuid().GetHashCode();
            return Create(id, null, executor, null, time, framerate);
        }

        /// <summary>
        /// 创建执行
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="callback"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(Action<float> executor, Action callback, float time = -1f, int framerate = 30)
        {
            long id = Guid.NewGuid().GetHashCode();
            return Create(id, null, executor, callback, time, framerate);
        }

        /// <summary>
        /// 创建执行
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="executor"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(Func<bool> predicate, Action<float> executor, float time = -1f, int framerate = 30)
        {
            long id = Guid.NewGuid().GetHashCode();
            return Create(id, predicate, executor, null, time, framerate);
        }

        /// <summary>
        /// 创建执行
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="executor"></param>
        /// <param name="callback"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(Func<bool> predicate, Action<float> executor, Action callback, float time = -1f, int framerate = 30)
        {
            long id = Guid.NewGuid().GetHashCode();
            return Create(id, predicate, executor, callback, time, framerate);
        }

        /// <summary>
        /// 创建执行
        /// </summary>
        /// <param name="id"></param>
        /// <param name="executor"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(long id, Action<float> executor, float time = -1f, int framerate = 30)
        {
            return Create(id, null, executor, null, time, framerate);
        }

        /// <summary>
        /// 创建带有中断条件的执行
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="executor"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(long id, Func<bool> predicate, Action<float> executor, float time = -1f, int framerate = 30)
        {
            return Create(id, predicate, executor, null, time, framerate);
        }

        /// <summary>
        /// 创建带有中断条件的执行
        /// </summary>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <param name="executor"></param>
        /// <param name="callback"></param>
        /// <param name="time"></param>
        /// <param name="framerate"></param>
        /// <returns></returns>
        public Ticker Create(long id, Func<bool> predicate, Action<float> executor, Action callback, float time = -1f, int framerate = 30)
        {
            Ticker ticker;
            if (m_tickers.ContainsKey(id))
            {
                ticker = m_tickers[id];
                ticker.AddChannel(TickChannel.Create(predicate, executor, callback, time, framerate));
            }
            else
            {
                ticker = Ticker.Create(id, predicate, executor, callback, time, framerate);
                m_tickers.Add(ticker.Id, ticker);
                m_updates.Enqueue(ticker.Id);
            }
            
            return ticker;
        }
        
        /// <summary>
        /// 移除执行
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
        {
            if (m_tickers.ContainsKey(id))
            {
                m_tickers.Remove(id);
            }
        }

        /// <summary>
        /// 获取执行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Ticker GetTicker(long id)
        {
            if (m_tickers.ContainsKey(id))
            {
                return m_tickers[id];
            }
            return null;
        }
        
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="id"></param>
        public void Play(long id)
        {
            if (m_tickers.ContainsKey(id))
            {
                m_tickers[id].Play();;
                if (!m_updates.Contains(id))
                {
                    m_updates.Enqueue(id);
                }
            }
        }
        
        /// <summary>
        /// 暂停执行
        /// </summary>
        /// <param name="id"></param>
        public void Pause(long id)
        {
            if (m_tickers.ContainsKey(id))
            {
                m_tickers[id].Pause();
            }
        }

        /// <summary>
        /// 执行全部
        /// </summary>
        public void PlayAll()
        {
            foreach (var kv in m_tickers)
            {
                kv.Value.Play();
                if (!m_updates.Contains(kv.Key))
                {
                    m_updates.Enqueue(kv.Key);
                }
            }
        }

        /// <summary>
        /// 暂停全部
        /// </summary>
        public void PauseAll()
        {
            foreach (var kv in m_tickers)
            {
                kv.Value.Pause();
            }
        }
    }
}