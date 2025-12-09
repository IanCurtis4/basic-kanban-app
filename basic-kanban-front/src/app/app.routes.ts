import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { BoardsListComponent } from './features/boards/boards-list.component';
import { BoardDetailComponent } from './features/boards/board-detail.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'boards', pathMatch: 'full' },
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent }
    ]
  },
  {
    path: 'boards',
    canActivate: [authGuard],
    children: [
      { path: '', component: BoardsListComponent },
      { path: ':id', component: BoardDetailComponent }
    ]
  },
  { path: '**', redirectTo: 'boards' }
];
