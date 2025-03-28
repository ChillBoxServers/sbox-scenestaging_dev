﻿using System.Linq;
using Sandbox.MovieMaker;

namespace Editor.MovieMaker;

#nullable enable

internal class KeyframeHandle : GraphicsItem
{
	public DopeSheetTrack Track { get; }
	public TrackKeyframes Keyframes { get; }
	private Color HandleColor { get; }

	public Session Session => Keyframes.TrackWidget.TrackList.Session;

	public MovieTime Time { get; set; }
	public object? Value { get; set; }

	public InterpolationMode Interpolation { get; set; }

	public KeyframeHandle( TrackKeyframes keyframes ) : base( keyframes.DopeSheetTrack )
	{
		Track = keyframes.DopeSheetTrack;
		Keyframes = keyframes;

		HoverEvents = true;
		HandlePosition = new Vector2( 0.5f, 0.0f );
		Size = 16.0f;
		Cursor = CursorShape.Finger;
		Movable = true;
		Focusable = true;
		Selectable = true;

		HandleColor = keyframes.HandleColor;
	}

	protected override void OnMoved()
	{
		base.OnMoved();

		Time = Session.ScenePositionToTime( Position, ignore: SnapFlag.PlayHead );

		Keyframes.Write();

		UpdatePosition();

		Session.SetCurrentPointer( Time );
	}

	protected override void OnSelectionChanged()
	{
		base.OnSelectionChanged();

		if ( Selected && GraphicsView.SelectedItems.Count() <= 1 )
		{
			Track.OnSelected();
			Session.SetCurrentPointer( Time );
		}
	}

	protected override void OnPaint()
	{
		if ( !Track.Visible ) return;

		var c = Extensions.PaintSelectColor( HandleColor.WithAlpha( 0.5f ), HandleColor, DopeSheet.Colors.HandleSelected );
		var b = Extensions.PaintSelectColor( Color.Black, Theme.Blue, Theme.White );

		var w = Width;
		var h = Height;

		Paint.SetBrushAndPen( c );

		Extensions.PaintTriangle( Size * 0.5f, 10 );

		Paint.SetPen( c.WithAlphaMultiplied( 0.3f ) );
		Paint.DrawLine( new Vector2( Width * 0.5f, 0 ), new Vector2( Width * 0.5f, Height ) );
	}

	internal void UpdatePosition()
	{
		PrepareGeometryChange();

		Position = new Vector2( Session.TimeToPixels( Time ), 0 );
		Size = Track.Visible ? new Vector2( 16, Parent.Height ) : 0f;

		Track.Update();
	}

	protected override void OnMousePressed( GraphicsMouseEvent e )
	{
		base.OnMousePressed( e );

		if ( e.LeftMouseButton )
		{
			Session.Current!.SetCurrentPointer( Time );
		}
	}

	internal void Nudge( MovieTime v )
	{
		Time = MovieTime.Max( MovieTime.Zero, Time + v );

		UpdatePosition();
	}
}
