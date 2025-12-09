import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BoardService } from '../../core/services/board.service';
import { AuthService } from '../../core/services/auth.service';
import { Board } from '../../core/models/board.model';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-boards-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="boards-container">
      <nav class="navbar">
        <h1>Kanban Board</h1>
        <button (click)="logout()" class="logout-btn">Logout</button>
      </nav>

      <div class="content">
        <div class="header">
          <h2>My Boards</h2>
          <button (click)="showCreateForm = !showCreateForm" class="create-btn">
            + New Board
          </button>
        </div>

        <div *ngIf="showCreateForm" class="create-form">
          <input 
            [(ngModel)]="newBoard.title" 
            placeholder="Board title"
            type="text"
          />
          <textarea 
            [(ngModel)]="newBoard.description" 
            placeholder="Board description"
          ></textarea>
          <button (click)="createBoard()" [disabled]="!newBoard.title">Create</button>
          <button (click)="showCreateForm = false" class="cancel-btn">Cancel</button>
        </div>

        <div *ngIf="loading" class="loading">Loading boards...</div>

        <div *ngIf="boards.length === 0 && !loading" class="empty-state">
          <p>No boards yet. Create one to get started!</p>
        </div>

        <div class="boards-grid">
          <div *ngFor="let board of boards" class="board-card" (click)="viewBoard(board.id)">
            <h3>{{ board.title }}</h3>
            <p>{{ board.description }}</p>
            <small>Created: {{ formatDate(board.createdAt) }}</small>
            <div class="card-actions">
              <button (click)="deleteBoard($event, board.id)" class="delete-btn">Delete</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .boards-container {
      min-height: 100vh;
      background: #f5f5f5;
    }

    .navbar {
      background: #667eea;
      color: white;
      padding: 1rem 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .navbar h1 {
      margin: 0;
      font-size: 1.5rem;
    }

    .logout-btn {
      background: #764ba2;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      transition: background-color 0.3s;
    }

    .logout-btn:hover {
      background-color: #5c3a7a;
    }

    .content {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .create-btn {
      background: #667eea;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      transition: background-color 0.3s;
    }

    .create-btn:hover {
      background-color: #5568d3;
    }

    .create-form {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      margin-bottom: 2rem;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .create-form input,
    .create-form textarea {
      width: 100%;
      padding: 0.75rem;
      margin-bottom: 1rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-family: inherit;
      font-size: 1rem;
    }

    .create-form textarea {
      min-height: 100px;
      resize: vertical;
    }

    .create-form button {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      margin-right: 0.5rem;
    }

    .create-form button:first-of-type {
      background: #667eea;
      color: white;
      transition: background-color 0.3s;
    }

    .create-form button:first-of-type:hover:not(:disabled) {
      background-color: #5568d3;
    }

    .create-form button:disabled {
      background-color: #ccc;
      cursor: not-allowed;
    }

    .cancel-btn {
      background: #ddd;
      color: #333;
      transition: background-color 0.3s;
    }

    .cancel-btn:hover {
      background-color: #ccc;
    }

    .loading, .empty-state {
      text-align: center;
      padding: 2rem;
      color: #666;
      font-size: 1.1rem;
    }

    .boards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1.5rem;
    }

    .board-card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      cursor: pointer;
      transition: transform 0.3s, box-shadow 0.3s;
    }

    .board-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .board-card h3 {
      margin: 0 0 0.5rem 0;
      color: #333;
    }

    .board-card p {
      color: #666;
      margin: 0.5rem 0;
    }

    .board-card small {
      color: #999;
    }

    .card-actions {
      margin-top: 1rem;
      display: flex;
      gap: 0.5rem;
    }

    .delete-btn {
      background: #e74c3c;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.875rem;
      transition: background-color 0.3s;
      flex: 1;
    }

    .delete-btn:hover {
      background-color: #c0392b;
    }
  `]
})
export class BoardsListComponent implements OnInit, OnDestroy {
  boards: Board[] = [];
  loading = false;
  showCreateForm = false;
  newBoard = { title: '', description: '' };
  private destroy$ = new Subject<void>();

  constructor(
    private boardService: BoardService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadBoards();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBoards(): void {
    this.loading = true;
    this.boardService.getBoards()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (boards) => {
          this.boards = boards;
          this.loading = false;
        },
        error: (err) => {
          console.error('Failed to load boards', err);
          this.loading = false;
        }
      });
  }

  createBoard(): void {
    if (!this.newBoard.title.trim()) return;

    this.boardService.createBoard(this.newBoard)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (board) => {
          this.boards.unshift(board);
          this.newBoard = { title: '', description: '' };
          this.showCreateForm = false;
        },
        error: (err) => console.error('Failed to create board', err)
      });
  }

  deleteBoard(event: Event, boardId: string): void {
    event.stopPropagation();
    if (!confirm('Are you sure you want to delete this board?')) return;

    this.boardService.deleteBoard(boardId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.boards = this.boards.filter(b => b.id !== boardId);
        },
        error: (err) => console.error('Failed to delete board', err)
      });
  }

  viewBoard(boardId: string): void {
    this.router.navigate(['/boards', boardId]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}
