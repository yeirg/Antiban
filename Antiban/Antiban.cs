using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Antiban
{
    public sealed class Antiban
    {
        private List<EventMessage> _eventMessages = new();
        private void PushPriority0(EventMessage eventMessage)
        {
            _eventMessages.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            if (eventMessage.Id == 4)
            {
                Debug.WriteLine("7");
            }
            // Если пусто, просто добавить
            if (_eventMessages.Count == 0)
            {
                _eventMessages.Add(eventMessage);
                return;
            }

            // ПОСЛЕДНЕЕ СООБЩЕНИЕ НА СЕГОДНЯ
            var last = _eventMessages.LastOrDefault(x => x.DateTime.Date == eventMessage.DateTime.Date);
            // ПОСЛЕДНЕЕ СООБЩЕНИЕ НА ЭТОТ НОМЕР
            var lastSamePhone = _eventMessages.LastOrDefault(x =>
                x.DateTime.Date == eventMessage.DateTime.Date && x.Phone == eventMessage.Phone);
            // СМОТРИМ. СООБЩЕНИЕ ЗА СЕГОДНЯ ОТ ЭТОГО НОМЕРА, ЕСТЬ РАССТОЯНИЕ МЕНЬШЕ 1 МИНУТЫ?
            if (lastSamePhone != null && eventMessage.DateTime.Subtract(lastSamePhone.DateTime).TotalMinutes < 1)
            {
                eventMessage.DateTime = lastSamePhone.DateTime.AddMinutes(1);
                _eventMessages.Add(eventMessage);
                return;
            }
            
            // Если такого не существует
            // Просто добавить
            if (last == null)
            {
                _eventMessages.Add(eventMessage);
                return;
            }
            var lastDiff = eventMessage.DateTime - last.DateTime;
            // Номер совпадает, расстояние меньше 1 минуты
            // Добавить 1 минуту
            if (last.Phone == eventMessage.Phone && lastDiff.TotalMinutes < 1)
            {
                eventMessage.DateTime = last.DateTime.AddMinutes(1);
                _eventMessages.Add(eventMessage);
                return;
            }
            // Номер совпадает, расстояние больше 1 минуты
            // Просто добавить
            if (last.Phone == eventMessage.Phone)
            {
                _eventMessages.Add(eventMessage);
                return;
            }
            // Если последнее сообщение пришло на другой номер
            // Если расстояние меньше 10 секунд, то добавить 10 секунд
            if (last.Phone != eventMessage.Phone && lastDiff.TotalSeconds < 10)
            {
                eventMessage.DateTime = last.DateTime.AddSeconds(10);
                _eventMessages.Add(eventMessage);
                return;
            }
            // Если последнее сообщение пришло на другой номер
            // Просто добавить
            if (last.Phone != eventMessage.Phone)
            {
                _eventMessages.Add(eventMessage);
                return;
            }
            // БИСМИЛЛЯ
            // ЕСЛИ ПРОШЛО МЕНЬШЕ МИНУТЫ С ТОГО ЖЕ НОМЕРА И МЕНЬШЕ 10 СЕКУНД С ПОСЛЕДНЕГО ДРУГОГО НОМЕРА
            // ТО ДОБАВИТЬ 1 МИНУТУ(?) ИЛИ 10 СЕКУНД(?)
            Environment.Exit(-1);
        }

        private void PushPriority1(EventMessage eventMessage)
        {
            _eventMessages.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            
            // Если пусто, просто добавить
            if (_eventMessages.Count == 0)
            {
                _eventMessages.Add(eventMessage);
                return;
            }
            
            // Существующие сообщения на тот же номер, вне зависимости от приоритета
            var existing = _eventMessages.Where(x => x.Phone == eventMessage.Phone).ToList();
            var existingLast = existing.Last();
            // Существующие сообщения на тот же номер, с приоритетом
            var existingPriority = existing.Where(x => x.Priority == 1).ToList();
            // Если таких нет
            if (existingPriority.Count == 0)
            {
                // Проверяем есть ли расстояние в 1 минуту
                var lastDiff = eventMessage.DateTime - existingLast.DateTime;
                // Добавляем минуту, если такого не наблюдается
                if (lastDiff.TotalMinutes < 1)
                {
                    eventMessage.DateTime = existingLast.DateTime.AddMinutes(1);
                    _eventMessages.Add(eventMessage);
                    return;
                }
                // Иначе просто добавляем
                else
                {
                    _eventMessages.Add(eventMessage);
                    return;
                }
            }
            // Иначе значит, что сообщение с приоритетом 1 существует
            else
            {
                var priorityDiff = eventMessage.DateTime - existingPriority.Last().DateTime;
                // Если расстояние меньше 1 дня, то добавляем 1 день
                if (priorityDiff.TotalDays < 1)
                {
                    eventMessage.DateTime = existingPriority.Last().DateTime.AddDays(1);
                    _eventMessages.Add(eventMessage);
                    return;
                }
                // Иначе просто добавляем
                else
                {
                    _eventMessages.Add(eventMessage);
                }
            }
        }
        
        
        /// <summary>
        /// Добавление сообщений в систему, для обработки порядка сообщений
        /// </summary>
        /// <param name="eventMessage"></param>
        public void PushEventMessage(EventMessage eventMessage)
        {
            if (eventMessage.Priority == 0)
            {
                PushPriority0(eventMessage);
                return;
            }
            if (eventMessage.Priority == 1)
            {
                PushPriority1(eventMessage);
                return;
            }





            /*foreach (var x in _eventMessages)
            {
                if (eventMessage.Priority == 1 && _eventMessages.Any(y => x.Phone == eventMessage.Phone && x.Priority == 1 && eventMessage.DateTime.Subtract(x.DateTime).TotalDays < 1))
                {
                    eventMessage.DateTime = x.DateTime.AddDays(1);
                    break;
                }
                if (x.Phone == eventMessage.Phone && eventMessage.DateTime.Subtract(x.DateTime).TotalMinutes < 1)
                {
                    eventMessage.DateTime = x.DateTime.AddMinutes(1);
                    break;
                }

            }


            /*if (_eventMessages.Any(x => x.Phone == eventMessage.Phone && x.Priority == 1))
            {
                eventMessage.DateTime = eventMessage.DateTime.AddDays(1);
            }#1#
            _eventMessages.Add(eventMessage);*/
        }

        /// <summary>
        /// Вовзращает порядок отправок сообщений
        /// </summary>
        /// <returns></returns>
        public List<AntibanResult> GetResult()
        {
            _eventMessages = _eventMessages.OrderBy(x => x.DateTime).ToList();
            var result = new List<AntibanResult>();
            
            foreach (var eventMessage in _eventMessages)
            {
                result.Add(new AntibanResult
                {
                    SentDateTime = eventMessage.DateTime,
                    EventMessageId = eventMessage.Id
                });
            }
            
            return result;
        }
        
        private class XDateTimeComparer : IComparer<EventMessage> {
            public int Compare(EventMessage? x, EventMessage? y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }
                
                return x.DateTime.CompareTo(y.DateTime);
            }
        }
    }
    
    
}
