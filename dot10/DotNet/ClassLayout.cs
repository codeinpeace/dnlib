﻿using System;
using dot10.DotNet.MD;

namespace dot10.DotNet {
	/// <summary>
	/// A high-level representation of a row in the ClassLayout table
	/// </summary>
	public abstract class ClassLayout : IMDTokenProvider {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

		/// <inheritdoc/>
		public MDToken MDToken {
			get { return new MDToken(Table.ClassLayout, rid); }
		}

		/// <summary>
		/// From column ClassLayout.PackingSize
		/// </summary>
		public abstract ushort PackingSize { get; set; }

		/// <summary>
		/// From column ClassLayout.ClassSize
		/// </summary>
		public abstract uint ClassSize { get; set; }

		/// <summary>
		/// From column ClassLayout.Parent
		/// </summary>
		public abstract TypeDef Parent { get; set; }
	}

	/// <summary>
	/// A ClassLayout row created by the user and not present in the original .NET file
	/// </summary>
	public class ClassLayoutUser : ClassLayout {
		ushort packingSize;
		uint classSize;
		TypeDef parent;

		/// <inheritdoc/>
		public override ushort PackingSize {
			get { return packingSize; }
			set { packingSize = value; }
		}

		/// <inheritdoc/>
		public override uint ClassSize {
			get { return classSize; }
			set { classSize = value; }
		}

		/// <inheritdoc/>
		public override TypeDef Parent {
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public ClassLayoutUser() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Parent</param>
		/// <param name="packingSize">PackingSize</param>
		/// <param name="classSize">ClassSize</param>
		public ClassLayoutUser(TypeDef parent, ushort packingSize, uint classSize) {
			this.packingSize = packingSize;
			this.classSize = classSize;
			this.parent = parent;
		}
	}

	/// <summary>
	/// Created from a row in the ClassLayout table
	/// </summary>
	sealed class ClassLayoutMD : ClassLayout {
		/// <summary>The module where this instance is located</summary>
		ModuleDefMD readerModule;
		/// <summary>The raw table row. It's null until <see cref="InitializeRawRow"/> is called</summary>
		RawClassLayoutRow rawRow;

		UserValue<ushort> packingSize;
		UserValue<uint> classSize;
		UserValue<TypeDef> parent;

		/// <inheritdoc/>
		public override ushort PackingSize {
			get { return packingSize.Value; }
			set { packingSize.Value = value; }
		}

		/// <inheritdoc/>
		public override uint ClassSize {
			get { return classSize.Value; }
			set { classSize.Value = value; }
		}

		/// <inheritdoc/>
		public override TypeDef Parent {
			get { return parent.Value; }
			set { parent.Value = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>ClassLayout</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is <c>0</c> or &gt; <c>0x00FFFFFF</c></exception>
		public ClassLayoutMD(ModuleDefMD readerModule, uint rid) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (rid == 0 || rid > 0x00FFFFFF)
				throw new ArgumentException("rid");
			if (readerModule.TablesStream.Get(Table.ClassLayout).Rows < rid)
				throw new BadImageFormatException(string.Format("ClassLayout rid {0} does not exist", rid));
#endif
			this.rid = rid;
			this.readerModule = readerModule;
			Initialize();
		}

		void Initialize() {
			packingSize.ReadOriginalValue = () => {
				InitializeRawRow();
				return rawRow.PackingSize;
			};
			classSize.ReadOriginalValue = () => {
				InitializeRawRow();
				return rawRow.ClassSize;
			};
			parent.ReadOriginalValue = () => {
				InitializeRawRow();
				return readerModule.ResolveTypeDef(rawRow.Parent);
			};
		}

		void InitializeRawRow() {
			if (rawRow != null)
				return;
			rawRow = readerModule.TablesStream.ReadClassLayoutRow(rid);
		}
	}
}