import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ShortUrlsTable } from './short-urls-table';

describe('ShortUrlsTable', () => {
  let component: ShortUrlsTable;
  let fixture: ComponentFixture<ShortUrlsTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShortUrlsTable],
    }).compileComponents();

    fixture = TestBed.createComponent(ShortUrlsTable);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
