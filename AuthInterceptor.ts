import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { PublicClientApplication } from '@azure/msal-browser';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private msalInstance = new PublicClientApplication({
    auth: {
      clientId: 'YOUR_CLIENT_ID',
      authority: 'https://login.microsoftonline.com/YOUR_TENANT_ID',
    },
  });

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const tokenRequest = { scopes: ['User.Read'] };

    return from(this.msalInstance.acquireTokenSilent(tokenRequest)).pipe(
      switchMap((response) => {
        const token = response?.accessToken;
        if (token) {
          req = req.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`,
            },
          });
        }
        return next.handle(req);
      })
    );
  }
}
