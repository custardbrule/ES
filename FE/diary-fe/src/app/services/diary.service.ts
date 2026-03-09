import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import {
  AddSectionByDayRequest,
  AddSectionRequest,
  CreateDiaryRequest,
  Diary,
  DiaryViewModel,
  DiarySectionViewModel,
  GetDiariesParams,
  GetDiariesResponse,
} from '@src/app/models/diary.models';

@Injectable({
  providedIn: 'root',
})
export class DiaryService {
  private readonly baseUrl = `${environment.diaryApiUrl}/Diary`;

  constructor(private http: HttpClient) {}

  getDiaries(params: GetDiariesParams = {}): Observable<GetDiariesResponse> {
    let httpParams = new HttpParams();

    if (params.name) httpParams = httpParams.set('Name', params.name);
    if (params.authorId)
      httpParams = httpParams.set('AuthorId', params.authorId);
    if (params.page != null) httpParams = httpParams.set('Page', params.page);
    if (params.pageSize != null)
      httpParams = httpParams.set('PageSize', params.pageSize);

    return this.http.get<GetDiariesResponse>(`${this.baseUrl}`, {
      params: httpParams,
    });
  }

  addDiary(body: CreateDiaryRequest): Observable<Diary> {
    return this.http.post<Diary>(`${this.baseUrl}`, body);
  }

  getDiaryById(id: string): Observable<DiaryViewModel> {
    return this.http.get<DiaryViewModel>(`${this.baseUrl}/${id}`);
  }

  addSection(body: AddSectionRequest): Observable<DiarySectionViewModel> {
    return this.http.post<DiarySectionViewModel>(`${this.baseUrl}/section`, body);
  }

  addSectionByDay(body: AddSectionByDayRequest): Observable<DiarySectionViewModel> {
    return this.http.post<DiarySectionViewModel>(`${this.baseUrl}/section/day`, body);
  }
}
