declare module 'greedy-wrap' {
    function greedyWrap(
        text: string,
        options: {
            width: number,
        }
    ) : string;
    
    export = greedyWrap;
}