using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	// TODO rename OmokTurnState?
	[Serializable]
	public class OmokHistoryNode {
		/// <summary>
		/// Which turn this is in the history of the game
		/// </summary>
		public int turnValue;
		/// <summary>
		/// Which player's turn it is: who is allowed to place a piece right now?
		/// </summary>
		public byte whosTurnIsItNow;
		/// <summary>
		/// Whic game state this came from
		/// </summary>
		public OmokHistoryNode parentNode;
		/// <summary>
		/// What move caused this state to happen from the parent game state
		/// </summary>
		public OmokMove sourceMove = null;
		/// <summary>
		/// State of board, managed by this class and referenced by other classes.
		/// </summary>
		public OmokBoardState boardState;
		/// <summary>
		/// Analysis of the board state.
		/// TODO clear this after a turn changes, and recalculate it if values are needed
		/// </summary>
		private OmokBoardStateAnalysis _boardAnalysis;
		/// <summary>
		/// Next states of the game that can come from this state
		/// TODO rename nextStateEdges?
		/// </summary>
		public OmokMovePath[] movePaths = Array.Empty<OmokMovePath>();
		/// <summary>
		/// Whether or not this state has been traversed by the UI. If so, this should be shown as a potential state in the graph
		/// </summary>
		public bool traversed;

		public int Turn => turnValue;
		public bool Traversed => traversed;
		public int GetEdgeCount() => movePaths.Length;
		public OmokBoardStateAnalysis BoardAnalysis => _boardAnalysis != null ? _boardAnalysis
			: _boardAnalysis = new OmokBoardStateAnalysis(boardState);

		/// <summary>
		/// TODO remove analysis as an argument, this should be dynamically calculated when needed, and queued to be cleared when the turn ends, to save memory
		/// </summary>
		/// <param name="state"></param>
		/// <param name="parentNode"></param>
		/// <param name="whosTurnIsItNow"></param>
		/// <param name="analysis"></param>
		/// <param name="sourceMove"></param>
		public OmokHistoryNode(OmokBoardState state, OmokHistoryNode parentNode, byte whosTurnIsItNow, OmokMove sourceMove) {
			this.boardState = state;
			this.parentNode = parentNode;
			this.sourceMove = sourceMove;
			this.whosTurnIsItNow = whosTurnIsItNow;
			if (parentNode != null) {
				turnValue = parentNode.turnValue + 1;
			}
		}

		public OmokHistoryNode FindRoot() {
			OmokHistoryNode cursor = this;
			while (cursor.parentNode != null) {
				cursor = cursor.parentNode;
			}
			return cursor;
		}

		public bool IsDoneCalculating(OmokMove move) {
			OmokHistoryNode alreadyExistingNode = GetMove(move);
			if (alreadyExistingNode != null) {
				return !alreadyExistingNode.BoardAnalysis.IsDoingAnalysis && alreadyExistingNode.BoardAnalysis.scoring != null;
			}
			return false;
		}

		public NextStateMovementResult AddMoveIfNotAlreadyCalculating(OmokMove move, byte whosTurnIsItNow,
			Action<OmokHistoryNode> whatToDoWhenMoveCalculationFinishes, MonoBehaviour coroutineRunner, out OmokHistoryNode nextNode) {
			/// if the move is already here, AddCallBackOnFinish, return false
			OmokMovePath nextPath = new OmokMovePath(move);
			int index = Array.BinarySearch(movePaths, nextPath, OmokMovePath.Comparer.Instance);
			if (index >= 0) {
				nextPath = movePaths[index];
				nextNode = nextPath.nextNode;// GetMove(index);
				if (!nextNode.BoardAnalysis.IsDoingAnalysis) {
					return NextStateMovementResult.FinishedCalculating;
				}
				return NextStateMovementResult.StillCalculating;
			}
			/// create a new <see cref="OmokMovePath">, start doing analysis of the move, AddCallBackOnFinish
			OmokBoardState nextState = new OmokBoardState(boardState);
			nextState.TrySetState(move);
			nextNode = new OmokHistoryNode(nextState, this, whosTurnIsItNow, move);
			nextPath.nextNode = nextNode;
			nextPath.nextNode.BoardAnalysis.MarkDoingAnalysis(true);
			InsertMove(~index, nextPath);
			coroutineRunner.StartCoroutine(nextNode.BoardAnalysis.AnalyzeCoroutine(nextNode,
				whatToDoWhenMoveCalculationFinishes));
			if (index >= 0) {
				Debug.LogError($"{nextPath.move.coord} already in list? {index}");
				return NextStateMovementResult.Error;
			}
			return NextStateMovementResult.StartedCalculating;
		}

		public OmokMovePath GetEdge(int i) => movePaths[i];

		/// <summary>
		/// 
		/// </summary>
		/// <param name="move"></param>
		/// <returns></returns>
		public bool AddMoveReplaceIfItExists(OmokMovePath move) {
			int index = Array.BinarySearch(movePaths, move, OmokMovePath.Comparer.Instance);
			if (index >= 0) {
				movePaths[index] = move;
				return true;
			}
			InsertMove(~index, move);
			return false;
		}

		public void InsertMove(int index, OmokMovePath move) {
			Array.Resize(ref movePaths, movePaths.Length + 1);
			for (int i = movePaths.Length - 1; i > index; --i) {
				movePaths[i] = movePaths[i - 1];
			}
			movePaths[index] = move;
		}

		public int GetMoveIndex(OmokMove move) => Array.IndexOf(movePaths, move);

		public int GetMoveIndex(OmokHistoryNode nextNode) => Array.FindIndex(movePaths, edge => edge.nextNode == nextNode);

		public OmokHistoryNode GetMove(OmokMove move) {
			int index = GetMoveIndex(move);
			if (index < 0) {
				return null;
			}
			return GetMove(index);
		}

		public OmokHistoryNode GetMove(int index) {
			return movePaths[index].nextNode;
		}

		public MinMax CalculateScoreRangeForPaths(byte player) {
			MinMax minmax = MinMax.Impossible;
			for (int i = 0; i < movePaths.Length; ++i) {
				OmokMovePath movepath = movePaths[i];
				OmokBoardStateAnalysis nextBoardAnalysis = movepath.nextNode.BoardAnalysis;
				float[] scoring = nextBoardAnalysis.scoring;
				if (nextBoardAnalysis.IsDoingAnalysis) {
					//Debug.Log($"skipping {movepath.move.coord}");
					continue;
				}
				float score = OmokBoardStateAnalysis.SummarizeScore(player, scoring);
				//Debug.Log($"{movepath.move.coord} {score}     {scoring[0]} v {scoring[1]}");
				minmax.Update(score);
			}
			return minmax;
		}

		public override string ToString() => sourceMove != null ? $"{Turn}:{sourceMove.coord}" : "omok";
	}
}
