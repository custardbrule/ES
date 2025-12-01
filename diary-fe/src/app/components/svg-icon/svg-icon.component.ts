import { HttpClient } from '@angular/common/http';
import { Component, Input, OnChanges } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-svg-icon',
  imports: [],
  templateUrl: './svg-icon.component.html',
  styleUrl: './svg-icon.component.scss',
})
export class SvgIconComponent implements OnChanges {
  @Input() public name?: string;
  @Input() public hoverName?: string;

  public svgIcon: any;
  public hoverSvgIcon: any;
  public currentIcon: any;

  constructor(
    private httpClient: HttpClient,
    private sanitizer: DomSanitizer
  ) {}

  public ngOnChanges(): void {
    if (!this.name) {
      this.svgIcon = '';
      this.currentIcon = '';
      return;
    }

    this.httpClient
      .get(`assets/images/svgs/${this.name}.svg`, { responseType: 'text' })
      .subscribe((value) => {
        this.svgIcon = this.sanitizer.bypassSecurityTrustHtml(value);
        this.currentIcon = this.svgIcon;
      });

    if (this.hoverName) {
      this.httpClient
        .get(`assets/images/svgs/${this.hoverName}.svg`, { responseType: 'text' })
        .subscribe((value) => {
          this.hoverSvgIcon = this.sanitizer.bypassSecurityTrustHtml(value);
        });
    }
  }

  public onMouseEnter(): void {
    if (this.hoverSvgIcon) {
      this.currentIcon = this.hoverSvgIcon;
    }
  }

  public onMouseLeave(): void {
    this.currentIcon = this.svgIcon;
  }
}
