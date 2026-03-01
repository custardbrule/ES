import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { GetDiariesParams, GetDiariesResponse } from '../models/diary.models';

@Injectable({
  providedIn: 'root',
})
export class DiaryService {
  private readonly baseUrl = `${environment.diaryApiUrl}/Diary`;

  constructor(private http: HttpClient) {}

  getDiaries(params: GetDiariesParams = {}): Observable<GetDiariesResponse> {
    let httpParams = new HttpParams();

    if (params.name) httpParams = httpParams.set('Name', params.name);
    if (params.authorId) httpParams = httpParams.set('AuthorId', params.authorId);
    if (params.page != null) httpParams = httpParams.set('Page', params.page);
    if (params.pageSize != null) httpParams = httpParams.set('PageSize', params.pageSize);

    return this.http.get<GetDiariesResponse>(`${this.baseUrl}/GetDiaries`, { params: httpParams });
  }
}
