import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CardDto, CreateCardDto, UpdateCardDto } from '../models/card.model';

@Injectable({ providedIn: 'root' })
export class CardService {
  private api = `${environment.apiUrl}/api/cards`;

  constructor(private http: HttpClient) {}

  getByCardList(cardListId: string): Observable<CardDto[]> {
    return this.http.get<CardDto[]>(`${this.api}/cardlist/${cardListId}`);
  }

  getById(id: string): Observable<CardDto> {
    return this.http.get<CardDto>(`${this.api}/${id}`);
  }

  create(dto: CreateCardDto): Observable<CardDto> {
    return this.http.post<CardDto>(this.api, dto);
  }

  update(id: string, dto: UpdateCardDto): Observable<CardDto> {
    return this.http.put<CardDto>(`${this.api}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/${id}`);
  }
}
