export enum EDiaryVisibility {
  Public = 1,
  FriendsOnly = 2,
  Private = 3,
  Hidden = 4,
}

export interface Diary {
  id: string;
  name: string;
  description: string;
  authorId: string;
  visibility: EDiaryVisibility;
}

export interface GetDiariesParams {
  name?: string;
  authorId?: string;
  page?: number;
  pageSize?: number;
}

export interface GetDiariesResponse {
  items: Diary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateDiaryRequest {
  name: string;
  description?: string;
  authorId?: string;
  diaryVisibility: EDiaryVisibility;
}
