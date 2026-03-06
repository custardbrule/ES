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
  authorName: string;
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

export interface DiarySectionViewModel {
  id: string;
  detail: string;
  isPinned: boolean;
  eventTime: string;
}

export interface DiaryDayViewModel {
  date: string;
  sections: DiarySectionViewModel[];
}

export interface DiaryViewModel extends Diary {
  dailyDiaries: DiaryDayViewModel[];
}

export interface AddSectionRequest {
  diaryId: string;
  timeZoneId: string;
  detail: string;
  isPinned: boolean;
}

export interface CreateDiaryRequest {
  name: string;
  description?: string;
  authorId?: string;
  authorName?: string;
  diaryVisibility: EDiaryVisibility;
}
