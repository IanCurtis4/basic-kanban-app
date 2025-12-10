export interface CardDto {
  id: string;
  title: string;
  description: string;
  difficulty: number;
  estimatedStart?: string | null; // ISO string
  estimatedEnd?: string | null;
  actualStart?: string | null;
  actualEnd?: string | null;
  cardListId: string;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  order: number;
}

export interface CreateCardDto {
  title: string;
  description?: string;
  difficulty: number;
  estimatedStart?: string | null;
  estimatedEnd?: string | null;
  cardListId: string;
  assignedToUserId?: string | null;
}

export interface UpdateCardDto {
  title?: string;
  description?: string;
  difficulty?: number;
  estimatedStart?: string | null;
  estimatedEnd?: string | null;
  actualStart?: string | null;
  actualEnd?: string | null;
  assignedToUserId?: string | null;
  order?: number | null;
}
