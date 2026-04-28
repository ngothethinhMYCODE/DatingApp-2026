import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Member, MemberParams } from '../../../types/member';
import { MemberCard } from "../member-card/member-card";
import { PaginatedResult } from '../../../types/pagination';
import { Paginator } from "../../../shared/paginator/paginator";
import { FilterModal } from '../filter-modal/filter-modal';

@Component({
  selector: 'app-member-list',
  imports: [MemberCard, Paginator, FilterModal],
  templateUrl: './member-list.html',
  styleUrl: './member-list.css',
})
export class MemberList implements OnInit {
  @ViewChild('filterModal') modal!: FilterModal;
  private memberService=inject(MemberService);
  protected paginatedMembers=signal< PaginatedResult<Member>| null>(null);
  protected memberParams=new MemberParams();
  private updateParams= new MemberParams();

  constructor(){
    const filter= localStorage.getItem('filter');
    if(filter){
      this.memberParams=JSON.parse(filter)
      this.updateParams=JSON.parse(filter)
    }
  }
  ngOnInit(): void {
    this.loadMembers();
  }
  loadMembers(){
    this.memberService.getMembers(this.memberParams).subscribe({
      next: result=>{
        this.paginatedMembers.set(result)
      }
    });
  }
  onPageChange(event: {pageNumber: number, pageSize:number}){
    this.memberParams.pageSize=event.pageSize;
    this.memberParams.pageNumber=event.pageNumber;
    this.loadMembers();
  }
  openModal(){
    this.modal.open();
  }
  onClose(){
    console.log('Modal closed');
  }
  onFilterChange(data: MemberParams){
    this.memberParams={...data};
    this.updateParams={...data};
    this.loadMembers();
  }
  resetFilters(){
    this.memberParams= new MemberParams();
    this.updateParams= new MemberParams();
    this.modal.memberParams.set(new MemberParams());
    this.loadMembers();
  }
  get displayMessage(): string{
    const defaultParams=new MemberParams();
    const filter: string[]=[];
    if(this.updateParams.gender){
      filter.push(this.updateParams.gender+'s')
    }else{
      filter.push('Males', 'Females');
    }
    if(this.updateParams.minAge!== defaultParams.minAge || this.updateParams.maxAge!== defaultParams.maxAge){
      filter.push(` ages ${this.updateParams.minAge}-${this.updateParams.maxAge}`)
    }
    filter.push(this.updateParams.orderBy==='lastActive' ? 'Recently active': 'Newest members');
    return filter.length>0 ? `Selected : ${filter.join('   |  ')}`: 'All members'
  }
}
