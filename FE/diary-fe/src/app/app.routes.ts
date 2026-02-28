import { Routes } from '@angular/router';

import { HomePageComponent } from '@pages/home-page/home-page.component';
import { NotFoundComponent } from '@pages/not-found/not-found.component';
import { DiaryPageComponent } from '@pages/diary-page/diary-page.component';
import { CallbackComponent } from '@pages/callback/callback.component';
import { ROUTE_DEF } from '@src/shared/constants/route-const';

export const routes: Routes = [
  { path: ROUTE_DEF.home.base, component: HomePageComponent },
  { path: ROUTE_DEF.collection.base, component: DiaryPageComponent },
  { path: 'callback', component: CallbackComponent },
  { path: 'signout-callback-oidc', component: HomePageComponent },
  { path: '**', component: NotFoundComponent },
];