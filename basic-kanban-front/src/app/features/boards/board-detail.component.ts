import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BoardService } from '../../core/services/board.service';
import { Board } from '../../core/models/board.model';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="board-detail-container">
      <nav class="navbar">
        <button (click)="goBack()" class="back-btn">← Back to Boards</button>
        <h1>{{ board?.title }}</h1>
      </nav>

      <div class="content" *ngIf="board">
        <p>{{ board.description }}</p>
        <small>Created: {{ formatDate(board.createdAt) }}</small>
        <p class="placeholder">📋 Card Lists and Cards will appear here in next MVP</p>
      </div>

      <div *ngIf="loading" class="loading">Loading board...</div>
    </div>
  `,
  styles: [`
    .board-detail-container {
      min-height: 100vh;
      background: #f5f5f5;
    }

    .navbar {
      background: #667eea;
      color: white;
      padding: 1rem 2rem;
      display: flex;
      align-items: center;
      gap: 2rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .back-btn {
      background: rgba(255, 255, 255, 0.2);
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      transition: background-color 0.3s;
    }

    .back-btn:hover {
      background-color: rgba(255, 255, 255, 0.3);
    }

    .navbar h1 {
      margin: 0;
      flex: 1;
    }

    .content {
      max-width: 1200px;
      margin: 2rem auto;
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .placeholder {
      text-align: center;
      padding: 4rem 2rem;
      color: #999;
      font-size: 1.2rem;
    }

    .loading {
      text-align: center;
      padding: 2rem;
      color: #666;
      font-size: 1.1rem;
    }
  `]
})
export class BoardDetailComponent implements OnInit {
  board: Board | null = null;
  loading = false;

  constructor(
    private boardService: BoardService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const boardId = this.route.snapshot.paramMap.get('id');
    if (boardId) {
      this.loadBoard(boardId);
    }
  }

  loadBoard(boardId: string): void {
    this.loading = true;
    this.boardService.getBoardById(boardId).subscribe({
      next: (board) => {
        this.board = board;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load board', err);
        this.loading = false;
        this.router.navigate(['/boards']);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/boards']);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}
