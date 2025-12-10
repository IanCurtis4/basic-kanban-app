import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="register-container">
      <div class="register-card">
        <h2>Create Account</h2>
        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Full Name</label>
            <input 
              type="text" 
              formControlName="fullName" 
              placeholder="Enter your full name"
              [class.error]="isFieldInvalid('fullName')"
            />
            <span *ngIf="isFieldInvalid('fullName')" class="error-message">
              Full name is required
            </span>
          </div>

          <div class="form-group">
            <label>Username</label>
            <input 
              type="text" 
              formControlName="userName" 
              placeholder="Choose a username"
              [class.error]="isFieldInvalid('userName')"
            />
            <span *ngIf="isFieldInvalid('userName')" class="error-message">
              Username is required
            </span>
          </div>

          <div class="form-group">
            <label>Email</label>
            <input 
              type="email" 
              formControlName="email" 
              placeholder="Enter your email"
              [class.error]="isFieldInvalid('email')"
            />
            <span *ngIf="isFieldInvalid('email')" class="error-message">
              Valid email is required
            </span>
          </div>

          <div class="form-group">
            <label>Password</label>
            <input 
              type="password" 
              formControlName="password" 
              placeholder="Min 8 chars, 1 uppercase, 1 digit"
              [class.error]="isFieldInvalid('password')"
            />
            <span *ngIf="isFieldInvalid('password')" class="error-message">
              Password must be at least 8 characters with uppercase and digits
            </span>
          </div>

          <div class="form-group">
            <label>Confirm Password</label>
            <input 
              type="password" 
              formControlName="confirmPassword" 
              placeholder="Confirm your password"
              [class.error]="isFieldInvalid('confirmPassword')"
            />
            <span *ngIf="isFieldInvalid('confirmPassword')" class="error-message">
              Please confirm your password
            </span>
          </div>

          <button type="submit" [disabled]="registerForm.invalid || loading">
            {{ loading ? 'Creating account...' : 'Sign Up' }}
          </button>

          <span *ngIf="error" class="error-message global">{{ error }}</span>
        </form>

        <p class="login-link">
          Already have an account? <a routerLink="/auth/login">Login</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .register-card {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 400px;
    }

    h2 {
      text-align: center;
      margin-bottom: 2rem;
      color: #333;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    label {
      display: block;
      margin-bottom: 0.5rem;
      color: #333;
      font-weight: 500;
    }

    input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
      transition: border-color 0.3s;
    }

    input:focus {
      outline: none;
      border-color: #667eea;
    }

    input.error {
      border-color: #e74c3c;
    }

    .error-message {
      display: block;
      color: #e74c3c;
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    .error-message.global {
      display: block;
      margin-bottom: 1rem;
      text-align: center;
    }

    button {
      width: 100%;
      padding: 0.75rem;
      background: #667eea;
      color: white;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      cursor: pointer;
      font-weight: 600;
      transition: background-color 0.3s;
    }

    button:hover:not(:disabled) {
      background-color: #5568d3;
    }

    button:disabled {
      background-color: #ccc;
      cursor: not-allowed;
    }

    .login-link {
      text-align: center;
      margin-top: 1rem;
      color: #666;
    }

    .login-link a {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
    }

    .login-link a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      userName: ['', [Validators.required]],
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/(?=.*[A-Z])(?=.*\d)/)
      ]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.loading = true;
    this.error = null;

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.router.navigate(['/boards']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Registration failed';
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}
