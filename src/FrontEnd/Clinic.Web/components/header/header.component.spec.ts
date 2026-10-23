import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { HeaderComponent } from './header.component';

describe('HeaderComponent', () => {
  let fixture: ComponentFixture<HeaderComponent>;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule.withRoutes([
        { path: '', redirectTo: 'accueil', pathMatch: 'full' },
        { path: 'accueil', children: [] },
        { path: 'patients', children: [] },
        { path: 'patients/:id', children: [] },
        { path: 'contact', children: [] },
      ])],
      declarations: [HeaderComponent],
    });
    fixture = TestBed.createComponent(HeaderComponent);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  const activeLinks = (): string[] =>
    [...fixture.nativeElement.querySelectorAll('nav a.active')].map((a: HTMLElement) => a.textContent!.trim());

  it('highlights the link for the current route', fakeAsync(() => {
    router.navigateByUrl('/patients'); tick(); fixture.detectChanges();
    expect(activeLinks()).toEqual(['Patients']);
  }));

  it('moves the highlight when navigating back', fakeAsync(() => {
    router.navigateByUrl('/patients'); tick();
    router.navigateByUrl('/accueil'); tick(); fixture.detectChanges();
    expect(activeLinks()).toEqual(['Accueil']);
  }));

  it('highlights Accueil on the root url', fakeAsync(() => {
    router.navigateByUrl('/'); tick(); fixture.detectChanges();
    expect(activeLinks()).toEqual(['Accueil']);
  }));

  it('keeps Patients highlighted on a patient page, the way back to the list', fakeAsync(() => {
    router.navigateByUrl('/patients/2'); tick(); fixture.detectChanges();
    expect(activeLinks()).toEqual(['Patients']);
  }));

  it('highlights Contact on the contact page', fakeAsync(() => {
    router.navigateByUrl('/contact'); tick(); fixture.detectChanges();
    expect(activeLinks()).toEqual(['Contact']);
  }));
});
