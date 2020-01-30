using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EFCore.Extensions
{
	/// <summary>
	/// Optional base class of IEntity to add all base functionality
	/// </summary>
	public abstract class BaseEntity : IEntity
	{
		/// <summary />
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		/// <summary />
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

		/// <summary />
		protected virtual void OnPropertyChanging(System.ComponentModel.PropertyChangingEventArgs e)
		{
			if (this.PropertyChanging != null)
				this.PropertyChanging(this, e);
		}

		/// <summary />
		protected virtual void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, e);
		}

		/// <summary>
		/// The event argument type of all property setters before the property is changed
		/// </summary>
		public partial class ChangingEventArgs<T> : ChangedEventArgs<T>
		{
			/// <summary>
			/// Initializes a new instance of the ChangingEventArgs class
			/// </summary>
			/// <param name="newValue">The new value of the property being set</param>
			/// <param name="propertyName">The name of the property being set</param>
			public ChangingEventArgs(T newValue, string propertyName)
				: base(newValue, propertyName)
			{
			}
			/// <summary>
			/// Determines if this operation is cancelled.
			/// </summary>
			public bool Cancel { get; set; }
		}

		/// <summary />
		public class EntityEventArgs : System.EventArgs
		{
			/// <summary />
			public IEntity Entity { get; set; }
		}

		/// <summary />
		public class EntityListEventArgs : System.EventArgs
		{
			/// <summary />
			public IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> List { get; set; }
		}

		/// <summary>
		/// The event argument type of all property setters after the property is changed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public partial class ChangedEventArgs<T> : System.ComponentModel.PropertyChangingEventArgs
		{
			/// <summary>
			/// Initializes a new instance of the ChangingEventArgs class
			/// </summary>
			/// <param name="newValue">The new value of the property being set</param>
			/// <param name="propertyName">The name of the property being set</param>
			public ChangedEventArgs(T newValue, string propertyName)
				: base(propertyName)
			{
				this.Value = newValue;
			}
			/// <summary>
			/// The new value of the property
			/// </summary>
			public T Value { get; set; }
		}

		protected void SetProperty<T>(string propertyName, T value, ref T setting)
		{
			if (EqualityComparer<T>.Default.Equals(setting, value)) return;
			var eventArg = new ChangingEventArgs<T>(value, nameof(propertyName));
			this.OnPropertyChanging(eventArg);
			if (eventArg.Cancel) return;
			setting = eventArg.Value;
			this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(propertyName)));
		}

	}
}