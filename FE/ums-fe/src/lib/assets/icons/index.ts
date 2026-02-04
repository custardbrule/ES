export {default as NextSvg} from './next.svg?raw';
export {default as PrevSvg} from './prev.svg?raw';

interface ValidatorBuilder<T>{
    name: string,
    data: string
}

type Validator<S> = {
    [K in keyof S]: S[K];
}