export interface Board {
  id: string;
  title: string;
  description: string;
  ownerId: string;
  createdAt: string;
  updatedAt?: string;
  isArchived: boolean;
}

export interface CreateBoardRequest {
  title: string;
  description: string;
}

export interface UpdateBoardRequest {
  title?: string;
  description?: string;
}
