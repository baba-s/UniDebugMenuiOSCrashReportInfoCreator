using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace Kogane.DebugMenu
{
	/// <summary>
	/// iOS のクラッシュ情報を表示するクラス
	/// </summary>
	public sealed class iOSCrashReportInfoCreator : ListCreatorBase<ActionData>
	{
		//==============================================================================
		// 定数(static readonly)
		//==============================================================================
		private static readonly string NL          = Environment.NewLine;
		private static readonly string TIME_FORMAT = "yyyy/MM/dd HH:mm:ss";

		//==============================================================================
		// クラス
		//==============================================================================
		private sealed class CrashData
		{
			public string Time       { get; }
			public string Summary    { get; }
			public string ListText   { get; }
			public string DetailText { get; }

			public CrashData( CrashReport report )
			{
				var text = report.text;

				Time       = report.time.ToString( TIME_FORMAT );
				Summary    = text.Split( new[] { NL }, StringSplitOptions.None ).FirstOrDefault();
				ListText   = $"{Time}: {Summary}";
				DetailText = $"{Time}{NL} {NL}{text}";
			}
		}

		//==============================================================================
		// 変数
		//==============================================================================
		private ActionData[] m_list;

		//==============================================================================
		// プロパティ
		//==============================================================================
		public override int Count => m_list.Length;

		public override ActionData[] OptionActionList =>
			new[]
			{
				new ActionData( "テスト", () => Utils.ForceCrash( ForcedCrashCategory.AccessViolation ) ),
				new ActionData
				(
					"削除", () =>
					{
						CrashReport.RemoveAll();
						UpdateDisp();
					}
				),
			};

		//==============================================================================
		// 関数
		//==============================================================================
		/// <summary>
		/// リストの表示に使用するデータを作成します
		/// </summary>
		protected override void DoCreate( ListCreateData data )
		{
			m_list = CrashReport.reports
					.Take( CrashReport.reports.Length - 1 ) // 末尾のレポートが重複していたので無視
					.OrderByDescending( x => x.time )
					.Select( x => new CrashData( x ) )
					.Where( x => data.IsMatch( x.Time, x.Summary ) )
					.Select( x => new ActionData( x.ListText, () => OpenAdd( DMType.TEXT_TAB_6, new SimpleInfoCreator( x.DetailText ) ) ) )
					.ToArray()
				;

			if ( data.IsReverse )
			{
				Array.Reverse( m_list );
			}
		}

		/// <summary>
		/// 指定されたインデックスの要素の表示に使用するデータを返します
		/// </summary>
		protected override ActionData DoGetElemData( int index )
		{
			return m_list.ElementAtOrDefault( index );
		}
	}
}