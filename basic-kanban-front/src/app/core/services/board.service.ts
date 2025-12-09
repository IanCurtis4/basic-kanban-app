import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { Board, CreateBoardRequest, UpdateBoardRequest } from '../models/board.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BoardService {
  private apiUrl = `${environment.apiUrl}/api/boards`;
  private boardsSubject = new BehaviorSubject<Board[]>([]);
  public boards$ = this.boardsSubject.asObservable();

  constructor(private http: HttpClient) {}

  getBoards(): Observable<Board[]> {
    return this.http.get<Board[]>(this.apiUrl);
  }

  getBoardById(id: string): Observable<Board> {
    return this.http.get<Board>(`${this.apiUrl}/${id}`);
  }

  createBoard(board: CreateBoardRequest): Observable<Board> {
    return this.http.post<Board>(this.apiUrl, board);
  }

  updateBoard(id: string, board: UpdateBoardRequest): Observable<Board> {
    return this.http.put<Board>(`${this.apiUrl}/${id}`, board);
  }

  deleteBoard(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  loadBoards(): void {
    this.getBoards().subscribe({
      next: boards => this.boardsSubject.next(boards),
      error: error => console.error('Failed to load boards', error)
    });
  }
}
