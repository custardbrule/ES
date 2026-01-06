import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';

export interface Section {
  time: string;
  content: string;
  isPinned: boolean;
}

export interface DiaryDay {
  id: number;
  date: string;
  isExpanded: boolean;
  isLoading: boolean;
  sections: Section[];
}

@Injectable({
  providedIn: 'root'
})
export class DiaryMockService {
  private mockDays: DiaryDay[] = [
    { id: 1, date: '24/11/2024', isExpanded: false, isLoading: false, sections: [] },
    { id: 2, date: '23/11/2024', isExpanded: false, isLoading: false, sections: [] },
    { id: 3, date: '22/11/2024', isExpanded: false, isLoading: false, sections: [] },
    { id: 4, date: '21/11/2024', isExpanded: false, isLoading: false, sections: [] },
    { id: 5, date: '20/11/2024', isExpanded: false, isLoading: false, sections: [] },
  ];

  private mockSections: { [dayId: number]: Section[] } = {
    1: [
      {
        time: '08:00',
        content: 'Started the day with a refreshing morning walk around the neighborhood. The weather was absolutely perfect - clear blue skies, gentle breeze, and the temperature was just right. I felt incredibly energized and motivated to tackle the day ahead. During the walk, I listened to an interesting podcast about productivity techniques and took some mental notes about implementing new strategies in my daily routine. The fresh air and movement really helped clear my mind and set a positive tone for the entire day.',
        isPinned: true
      },
      {
        time: '10:30',
        content: 'Had a highly productive team meeting that exceeded all expectations. We discussed several new project ideas and established realistic timelines for implementation. The brainstorming session was particularly creative, with everyone contributing innovative solutions to current challenges. We also addressed some technical debt and planned refactoring strategies for the upcoming sprint.',
        isPinned: false
      },
      {
        time: '14:00',
        content: 'Worked extensively on the diary feature implementation. Making excellent progress with Angular components, successfully creating reusable modules that will benefit the entire application. Implemented proper state management and ensured all components follow best practices. The architecture is coming together nicely, and I\'m particularly proud of the clean separation of concerns.',
        isPinned: false
      },
      {
        time: '16:45',
        content: 'Quick coffee break and casual chat with colleagues about the latest industry trends. We discussed emerging technologies, shared articles we\'d read recently, and exchanged ideas about potential improvements to our development workflow.',
        isPinned: false
      },
      {
        time: '18:30',
        content: 'Evening workout session at the gym. Focused on strength training and cardio. Feeling fantastic after a long day of intensive work. Exercise really helps maintain work-life balance and keeps energy levels high throughout the week.',
        isPinned: true
      },
    ],
    2: [
      {
        time: '09:00',
        content: 'Morning coffee ritual while planning the day ahead. Carefully reviewed my task list, prioritized items based on urgency and importance, and set clear, achievable goals. Used the first hour to organize thoughts and prepare mentally for the challenges ahead.',
        isPinned: false
      },
      {
        time: '12:00',
        content: 'Lunch break with colleagues turned into an engaging discussion about emerging technology trends, artificial intelligence advancements, and the future of web development. We shared perspectives on how these changes might impact our work and brainstormed ways to stay ahead of the curve. The conversation was both informative and inspiring.',
        isPinned: true
      },
      {
        time: '14:30',
        content: 'Deep dive into debugging a complex issue that had been puzzling the team for days. Finally found the root cause - it was a subtle race condition in our async code. The solution required careful refactoring, but it was worth it. This experience taught me valuable lessons about concurrent programming.',
        isPinned: false
      },
      {
        time: '16:00',
        content: 'Code review session with the team. This was particularly valuable - learned several new patterns and best practices from senior developers. Received constructive feedback on my recent pull request and made notes about areas for improvement. The collaborative learning environment in our team is truly exceptional.',
        isPinned: false
      },
      {
        time: '19:00',
        content: 'Evening relaxation with some light reading and planning for tomorrow. Reflected on today\'s accomplishments and identified areas where I can be more efficient.',
        isPinned: false
      },
    ],
    3: [
      {
        time: '07:30',
        content: 'Early morning yoga and meditation session. This practice has become essential for maintaining focus and mental clarity throughout busy workdays. Spent 45 minutes on various poses and breathing exercises, followed by 15 minutes of mindfulness meditation. The combination helps reduce stress and improves concentration significantly.',
        isPinned: false
      },
      {
        time: '11:00',
        content: 'Client presentation went exceptionally well! They were genuinely impressed with the new features we demonstrated, particularly the improved user interface and enhanced performance metrics. The interactive demo resonated strongly with stakeholders, and they provided enthusiastic approval to proceed with the next phase. Their positive feedback validated weeks of hard work by the entire team. We also discussed future enhancements and gathered valuable insights about their evolving requirements.',
        isPinned: true
      },
      {
        time: '15:30',
        content: 'Intensive deep work session focused on optimizing the backend API. Successfully identified and resolved several complex performance bottlenecks that were affecting response times. Implemented caching strategies, optimized database queries, and refactored some inefficient algorithms. The performance improvements are significant - reduced average response time by 40%. This required careful analysis, profiling, and systematic optimization. Very satisfied with the results.',
        isPinned: false
      },
      {
        time: '19:00',
        content: 'Reading an excellent new book about modern software architecture patterns and microservices design. The content is very insightful, offering fresh perspectives on scalability, maintainability, and system design. Taking detailed notes on concepts that could be applied to our current projects.',
        isPinned: false
      },
    ],
    4: [
      {
        time: '08:30',
        content: 'Team standup meeting to sync on progress. Everyone is on track with their sprint goals, which is encouraging. Discussed potential blockers and coordinated efforts for interdependent tasks. The team\'s communication and collaboration continue to improve.',
        isPinned: false
      },
      {
        time: '13:00',
        content: 'Rewarding pair programming session where I helped a junior developer debug a particularly tricky issue. Walking through the problem-solving process together was beneficial for both of us - they learned debugging techniques while I gained fresh perspectives on approaching problems. We eventually traced the bug to an unexpected side effect in a third-party library. The experience reminded me how valuable knowledge sharing is for team growth and how teaching others reinforces your own understanding.',
        isPinned: false
      },
      {
        time: '17:00',
        content: 'Attended an inspiring tech talk about cutting-edge approaches to modern web development. The speaker covered new frameworks, performance optimization techniques, and emerging best practices in the industry. Learned about several tools and methodologies that could potentially improve our development workflow. Made connections with other developers and exchanged ideas about implementation strategies.',
        isPinned: true
      },
    ],
    5: [
      {
        time: '09:30',
        content: 'Started working on an exciting new feature that presents interesting technical challenges. The requirements are complex but achievable, and I\'m genuinely excited about solving the architectural puzzles involved. Spent the morning planning the approach, sketching out component designs, and identifying potential pitfalls to avoid.',
        isPinned: false
      },
      {
        time: '12:30',
        content: 'Quick lunch followed by a refreshing walk around the nearby park. The change of scenery and fresh air work wonders for clearing mental fog and boosting creativity. Often my best ideas come during these walking breaks when my mind can wander freely.',
        isPinned: true
      },
      {
        time: '14:30',
        content: 'Intensive bug fixing marathon! Successfully resolved 5 critical issues that needed to be addressed before the upcoming release. Each bug required careful investigation, systematic debugging, and thorough testing to ensure the fixes didn\'t introduce new problems. The work was challenging but satisfying, especially seeing the issue count drop steadily throughout the afternoon.',
        isPinned: false
      },
      {
        time: '18:00',
        content: 'Wrapped up the week on a very high note! All sprint goals achieved, code quality maintained, and the team is in great shape heading into the weekend. Feeling accomplished and ready to recharge. Looking forward to next week\'s challenges with renewed energy and enthusiasm.',
        isPinned: false
      },
    ],
  };

  constructor() {}

  // Mock API call to get list of days
  getDays(): Observable<DiaryDay[]> {
    // Simulate API delay
    return of(this.mockDays).pipe(delay(300));
  }

  // Mock API call to get sections for a specific day
  getSectionsForDay(dayId: number): Observable<Section[]> {
    const sections = this.mockSections[dayId] || [];
    // Simulate API delay
    return of(sections).pipe(delay(500));
  }
}
