using GithubDisplay.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace GithubDisplay.Services
{
    public class Tracker<T> : INotifyCollectionChanged where T : INotifyPropertyChanged
    {
        public Tracker()
        {
            _equalityComparer = EqualityComparer<T>.Default;
        }

        public Tracker(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
        }

        public ObservableCollection<TrackedEntity<T>> Entities { get; private set; } = new ObservableCollection<TrackedEntity<T>>();

        IEqualityComparer<T> _equalityComparer;

        /// <summary>
        /// Adds an entity to the tracker list. If the entity changes based on the allowed things to track,
        /// then trigger the tracker.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        public void Add(T entity)
        {
            if (!IsTracked(entity))
            {
                Entities.Add(new TrackedEntity<T>(entity));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity));
            }
        }

        /// <summary>
        /// Causes the entity to update its property based on the new
        /// entity. Will add the entity if it doesn't exist.
        /// </summary>
        /// <param name="entity">The new entity</param>
        public void Update(T entity)
        {
            if (IsTracked(entity))
            {
                var oldEntity = Entities.First(e => e.Equals(entity, _equalityComparer));
                if (oldEntity.Entity.Merge(entity))
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, entity, oldEntity));
                }
            }
            else
            {
                Add(entity);
            }
        }

        /// <summary>
        /// Causes a bunch of entities to update their properties based on the new
        /// list of entities. Entity will add and remove from the original list.
        /// </summary>
        /// <param name="entities">The new entities</param>
        public void Update(IEnumerable<T> entities, bool removeNonexistentEntities = true)
        {
            if (removeNonexistentEntities)
            {
                var removals = Entities.Select(e => e.Entity).Except(entities, _equalityComparer);
                foreach (var entity in removals)
                {
                    Remove(entity);
                }
            }

            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        /// <summary>
        /// Removes the entity from the tracker list
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        public void Remove(T entity)
        {
            if (IsTracked(entity))
            {
                Entities.Remove(Entities.First(e => e.Equals(entity)));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entity));
            }
        }

        public bool IsTracked(T entity)
        {
            return Entities.Any(e => e.Equals(entity, _equalityComparer));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    public class TrackedEntity<T> where T : INotifyPropertyChanged
    {

        public TrackedEntity(T obj) 
            : this(obj, new string[] { }) { }

        public TrackedEntity(T obj, params string[] allowedProperties)
            : this(obj, allowedProperties.ToList()) { }

        public TrackedEntity(T obj, IEnumerable<string> allowedProperties)
        {
            Entity = obj;
            AllowedProperties = new List<string>(allowedProperties);
        }

        T _entity;

        public T Entity
        {
            get { return _entity; }
            set
            {
                if (_entity != null && _entity.Equals(value)) { return; }

                if (_entity != null)
                {
                    _entity.PropertyChanged -= _PropertyChanged;
                }

                _entity = value;

                if (_entity != null)
                {
                    _entity.PropertyChanged += _PropertyChanged;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is TrackedEntity<T> te)
            {
                return te == this;
            }

            if (obj is T t)
            {
                return t.Equals(Entity);
            }

            return false;
        }

        public bool Equals(T obj, IEqualityComparer<T> equalityComparer)
        {
            return equalityComparer.Equals(Entity, obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(_entity) * 397) ^ (AllowedProperties != null ? AllowedProperties.GetHashCode() : 0);
            }
        }

        public IList<string> AllowedProperties { get; } = new List<string>();

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        void _PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!AllowedProperties.Any())
            {
                PropertyChanged?.Invoke(sender, e);
            }
            else
            {
                if (AllowedProperties.Contains(e.PropertyName))
                {
                    PropertyChanged?.Invoke(sender, e);
                }
            }
        }
    }
}
